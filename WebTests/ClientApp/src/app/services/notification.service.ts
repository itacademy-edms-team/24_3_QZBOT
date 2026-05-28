import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})

export class NotificationService {
  private baseUrl = "https://localhost:44356/api/notification"

  constructor(
    private http: HttpClient
  ) { }

  getNotifications() {
    return this.http.get<Notification[]>(`${this.baseUrl}`, { withCredentials: true });
  }

  markAsRead(id: number) {
    return this.http.post(`${this.baseUrl}/read/${id}`, {}, { withCredentials: true });
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
