import { Component, computed, effect, ElementRef, inject, signal, viewChild } from '@angular/core';
import { ReportService } from '../services/report.service';
import { rxResource } from '@angular/core/rxjs-interop';
import {
  Chart,
  BarController,
  BarElement,
  CategoryScale,
  LinearScale,
  DoughnutController,
  ArcElement,
  Tooltip,
  Legend,
} from 'chart.js';
import { PeriodType } from '../model/report.model';

Chart.register(
  BarController,
  BarElement,
  CategoryScale,
  LinearScale,
  DoughnutController,
  ArcElement,
  Tooltip,
  Legend
);
type PeriodTab = 'weekly' | 'monthly'; // <-- this line

@Component({
  selector: 'app-report',
  imports: [],
  templateUrl: './report.html',
  styleUrl: './report.css',
})
export class Report {
  // private readonly reportService = inject(ReportService);

  // // Async data fetch -> rxResource (matches AbsenceService/TeacherService pattern elsewhere in the app)
  // summaryResource = rxResource({
  //   stream: () => this.reportService.getSummary(),
  // });

  // // Pure synchronous derivation from already-loaded data -> computed
  // maxTrendCount = computed(() => {
  //   const trend = this.summaryResource.value()?.absenceTrend ?? [];
  //   return Math.max(1, ...trend.map((p) => p.count)); // avoid divide-by-zero for empty/zero data
  // });

  // coverageRate = computed(() => {
  //   const summary = this.summaryResource.value();
  //   if (!summary) return 0;
  //   const total = summary.periodsPendingToday + summary.periodsAssignedToday;
  //   return total === 0 ? 100 : Math.round((summary.periodsAssignedToday / total) * 100);
  // });

  // barHeightPct(count: number): number {
  //   return (count / this.maxTrendCount()) * 100;
  // }

  // workloadBadgeClass(effectiveLoad: number): string {
  //   if (effectiveLoad >= 8) return 'badge-high';
  //   if (effectiveLoad >= 4) return 'badge-medium';
  //   return 'badge-low';
  // }

private readonly reportService = inject(ReportService);
  exporting = signal(false);
  exportError = signal<string | null>(null);

  trendCanvas = viewChild<ElementRef<HTMLCanvasElement>>('trendCanvas');
  coverageCanvas = viewChild<ElementRef<HTMLCanvasElement>>('coverageCanvas');
  workloadCanvas = viewChild<ElementRef<HTMLCanvasElement>>('workloadCanvas');
 
  periodType = signal<PeriodType>('week');
  offset = signal(1); // 1 = last completed period
 
  summaryResource = rxResource({
    params: () => ({ periodType: this.periodType(), offset: this.offset() }),
    stream: ({ params }) => this.reportService.getSummary(params.periodType, params.offset),
  });
 
  coverageRate = computed(() => {
    const s = this.summaryResource.value();
    if (!s) return 0;
    const total = s.periodsPendingToday + s.periodsAssignedToday;
    return total === 0 ? 100 : Math.round((s.periodsAssignedToday / total) * 100);
  });
 
  private trendChart?: Chart;
  private coverageChart?: Chart;
  private workloadChart?: Chart;
  private chartsReady = false;
 
  constructor() {
    effect(() => {
      const summary = this.summaryResource.value();
      if (summary && this.chartsReady) {
        this.buildCharts();
      }
    });
  }
 
  ngAfterViewInit(): void {
    this.chartsReady = true;
    if (this.summaryResource.value()) this.buildCharts();
  }
 
  ngOnDestroy(): void {
    this.trendChart?.destroy();
    this.coverageChart?.destroy();
    this.workloadChart?.destroy();
  }
 
  setPeriodType(type: PeriodType): void {
    this.periodType.set(type);
    this.offset.set(1); // reset to last completed period on tab switch
  }
 
  goBack(): void {
    this.offset.update(o => o + 1);
  }
 
  goForward(): void {
    if (this.offset() > 1) this.offset.update(o => o - 1);
  }
 
  private buildCharts(): void {
    const summary = this.summaryResource.value()!;
 
    // Trend chart
    const trendEl = this.trendCanvas()?.nativeElement;
    if (trendEl) {
      this.trendChart?.destroy();
      this.trendChart = new Chart(trendEl, {
        type: 'bar',
        data: {
          labels: summary.absenceTrend.map(p =>
            new Date(p.date).toLocaleDateString('en-GB', { weekday: 'short', day: 'numeric' })
          ),
          datasets: [{
            label: 'Absences',
            data: summary.absenceTrend.map(p => p.count),
            backgroundColor: '#5b8def',
            borderRadius: 6,
          }],
        },
        options: {
          responsive: true,
          plugins: { legend: { display: false } },
          scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } },
        },
      });
    }
 
    // Coverage doughnut
    const coverageEl = this.coverageCanvas()?.nativeElement;
    if (coverageEl) {
      this.coverageChart?.destroy();
      this.coverageChart = new Chart(coverageEl, {
        type: 'doughnut',
        data: {
          labels: ['Assigned', 'Pending'],
          datasets: [{
            data: [summary.periodsAssignedToday, summary.periodsPendingToday],
            backgroundColor: ['#27ae60', '#e74c3c'],
            borderWidth: 0,
          }],
        },
        options: {
          responsive: true,
          cutout: '70%',
          plugins: { legend: { position: 'bottom' } },
        },
      });
    }
 
    // Effective workload horizontal bar with rich tooltip
    const workloadEl = this.workloadCanvas()?.nativeElement;
    if (workloadEl) {
      this.workloadChart?.destroy();
      this.workloadChart = new Chart(workloadEl, {
        type: 'bar',
        data: {
          labels: summary.topSubstitutes.map(t => t.name),
          datasets: [{
            label: 'Effective Load',
            data: summary.topSubstitutes.map(t => t.effectiveLoad),
            backgroundColor: summary.topSubstitutes.map(t =>
              t.effectiveLoad >= 8 ? '#e74c3c' :
              t.effectiveLoad >= 4 ? '#f39c12' : '#27ae60'
            ),
            borderRadius: 4,
          }],
        },
        options: {
          indexAxis: 'y',
          responsive: true,
          plugins: {
            legend: { display: false },
            tooltip: {
              callbacks: {
                label: (ctx) => {
                  const t = summary.topSubstitutes[ctx.dataIndex];
                  return [
                    `Regular Load: ${t.regularLoad}`,
                    `Subs Taken:   ${t.subsTaken}`,
                    `Missed:       ${t.missedClasses}`,
                    `Effective:    ${t.effectiveLoad}`,
                  ];
                },
              },
            },
          },
          scales: { x: { beginAtZero: true, ticks: { stepSize: 1 } } },
        },
      });
    }
  }

  exportReport(): void {
    const today = new Date();
    let targetDate: Date;

    if (this.periodType() === 'month') {
      // offset 1 = last completed month, 2 = two months ago, etc.
      targetDate = new Date(today.getFullYear(), today.getMonth() - this.offset(), 1);
    } else {
      // Weekly view — find the Monday of the selected week, use its month
      const dayOfWeek = today.getDay() === 0 ? 7 : today.getDay();
      const lastMonday = new Date(today);
      lastMonday.setDate(today.getDate() - dayOfWeek + 1 - (this.offset() * 7));
      targetDate = new Date(lastMonday.getFullYear(), lastMonday.getMonth(), 1);
    }

    const month = `${targetDate.getFullYear()}-${String(targetDate.getMonth() + 1).padStart(2, '0')}`;

    this.exporting.set(true);
    this.exportError.set(null);

    this.reportService.exportMonthlyReport(month).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `SubstitutionReport_${month}.xlsx`;
        a.click();
        URL.revokeObjectURL(url);
        this.exporting.set(false);
      },
      error: () => {
        this.exportError.set('Export failed. Please try again.');
        this.exporting.set(false);
      },
    });
  }
}
