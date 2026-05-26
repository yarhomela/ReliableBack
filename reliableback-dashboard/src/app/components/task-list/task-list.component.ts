import {Component, OnInit, OnDestroy} from '@angular/core';
import {AsyncPipe} from '@angular/common';
import {TaskHubService, TaskStatusUpdate} from '../../services/task-hub.service';
import {Observable} from 'rxjs';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [AsyncPipe],
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.scss']
})
export class TaskListComponent implements OnInit, OnDestroy {
  readonly updates$: Observable<TaskStatusUpdate[]>;

  constructor(private readonly taskHub: TaskHubService) {
    this.updates$ = this.taskHub.updates$;
  }

  ngOnInit(): void {
    this.taskHub.startConnection();
  }

  ngOnDestroy(): void {
    this.taskHub.stopConnection();
  }

  formatTime(isoString: string): string {
    return new Date(isoString).toLocaleTimeString('uk-UA', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  }

  getStatusColor(status: string): string {
    const colors: Record<string, string> = {
      Pending: '#6c757d',
      Queued: '#0d6efd',
      Running: '#fd7e14',
      Completed: '#198754',
      Failed: '#dc3545',
      Retrying: '#ffc107',
      DeadLettered: '#6f42c1'
    };
    return colors[status] ?? '#6c757d';
  }
}
