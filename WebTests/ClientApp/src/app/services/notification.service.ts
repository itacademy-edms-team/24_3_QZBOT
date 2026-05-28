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

      await this.hubConnection.stop();

    }

    this.hubConnection =
      new signalR.HubConnectionBuilder()

        .withUrl(
          'https://localhost:44356/notificationHub',
          {
            accessTokenFactory: () =>
              localStorage.getItem('token') || ''
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

    await this.hubConnection.start();

    console.log("SignalR connected")
  }

  loadNotifications() {

    this.getNotifications()
      .subscribe(data => {

        this.notificationsSubject.next(data);

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
