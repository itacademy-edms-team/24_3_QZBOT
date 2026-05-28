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

  startConnection() {

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

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connected');
      })
      .catch(err => {
        console.error(err);
      });

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
  }

  loadNotifications() {

    this.getNotifications()
      .subscribe(data => {

        this.notificationsSubject.next(data);

      });
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
