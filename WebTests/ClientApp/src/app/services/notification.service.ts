import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class NotificationService {
  private baseUrl = "https://localhost:44356/api/notification"
  private hubConnection?: signalR.HubConnection;

  private notificationsSubject = new BehaviorSubject<Notification[]>([]);

  notification$ = this.notificationsSubject.asObservable();

  constructor(
    private http: HttpClient
  ) { }

  getNotifications() {
    return this.http.get<Notification[]>(`${this.baseUrl}`, { withCredentials: true });
  }

  markAsRead(id: number) {
    return this.http.post(`${this.baseUrl}/read/${id}`, {}, { withCredentials: true });
  }

  async startConnection() {

    if (this.hubConnection) {

      this.hubConnection.off('ReceiveNotification');
      this.hubConnection.off('NotificationDeleted');

      await this.hubConnection.stop();

      this.hubConnection = undefined;
    }

    this.hubConnection =
      new signalR.HubConnectionBuilder()

        .withUrl(
          'https://localhost:44356/notificationHub',
          {
            withCredentials: true
          })

        .withAutomaticReconnect()

        .build();

    this.hubConnection.on(
      'ReceiveNotification',
      (notification: Notification) => {

        const current =
          this.notificationsSubject.value;

        this.notificationsSubject.next([
          notification,
          ...current
        ]);
      });

    this.hubConnection.on(
      'NotificationDeleted',
      (id: number) => {

        const filtered =
          this.notificationsSubject.value
            .filter(n => n.id !== id);

        this.notificationsSubject.next(filtered);
      });

    try {

      await this.hubConnection.start();

      console.log('SignalR connected');

    } catch (err) {

      console.error(err);
    }
  }

  loadNotifications() {

    this.getNotifications()
      .subscribe(data => {

        const current =
          this.notificationsSubject.value;

        const merged = [
          ...data.filter(d =>
            !current.some(c => c.id === d.id)),
          ...current
        ];

        this.notificationsSubject.next(merged);

      });
  }

  async stopConnection() {

    if (this.hubConnection) {

      await this.hubConnection.stop();

    }
  }

  clearNotifications() {

    this.notificationsSubject.next([]);

  }
}

export interface Notification {
  id: number;
  userId: string;
  title: string;
  message: string;
  isRead: boolean;
  createdDate: string;
  link?: string;
}
