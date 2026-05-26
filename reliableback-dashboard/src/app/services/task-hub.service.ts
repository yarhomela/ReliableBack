import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TaskStatusUpdate {
  taskId:         string;
  previousStatus: string;
  newStatus:      string;
  occurredAt:     string;
}

@Injectable({ providedIn: 'root' })
export class TaskHubService {
  private hubConnection!: signalR.HubConnection;

  private readonly updatesSubject = new BehaviorSubject<TaskStatusUpdate[]>([]);
  public readonly updates$ = this.updatesSubject.asObservable();

  public startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl)
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connected'))
      .catch(err => console.error('SignalR connection error:', err));

    this.hubConnection.on('ReceiveTaskStatusUpdate',
      (update: TaskStatusUpdate) => {
        const current = this.updatesSubject.getValue();
        this.updatesSubject.next([update, ...current].slice(0, 50));
      });
  }

  public stopConnection(): void {
    this.hubConnection?.stop();
  }
}
