import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

export interface TaskStatusUpdate {
  taskId: string;
  previousStatus: string;
  newStatus: string;
  occurredAt: string;
}

@Injectable({ providedIn: 'root' })
export class TaskHubService {
  private hubConnection!: signalR.HubConnection;

  private updatesSubject = new BehaviorSubject<TaskStatusUpdate[]>([]);
  public updates$ = this.updatesSubject.asObservable();

  public startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5185/hubs/tasks')
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connected'))
      .catch(err => console.error('SignalR error:', err));

    this.hubConnection.on('ReceiveTaskStatusUpdate',
      (update: TaskStatusUpdate) => {
        const current = this.updatesSubject.getValue();
        // Зберігаємо останні 50 оновлень
        this.updatesSubject.next([update, ...current].slice(0, 50));
      });
  }

  public stopConnection(): void {
    this.hubConnection?.stop();
  }
}