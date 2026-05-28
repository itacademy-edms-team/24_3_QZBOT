import { Component, OnInit, HostListener } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { NotificationService, Notification } from '../services/notification.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  isScrolled = false;
  isExpanded = false;
  currentUserUsername: string = '';

  isNotificationsOpen = false;
  notifications: Notification[] = [];

  constructor(
    public authService: AuthService,
    private notificationService: NotificationService,
    private router: Router,
  ) { }

  ngOnInit() {
    this.authService.currentUser$.subscribe({
      next: (data) => {
        this.currentUserUsername = data || '';
      }
    })
  }

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.isScrolled = window.scrollY > 20;
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  toggleNotifications() {
    this.isNotificationsOpen =
      !this.isNotificationsOpen;

    if (this.isNotificationsOpen) {
      this.loadNotifications();
    }
  }

  loadNotifications() {
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        this.notifications = data;
      },
      error: (err) => {
        console.error(err);
      }
    })
  }

  openNotification(notification: Notification) {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe(() => {
        notification.isRead = true;
      })
    }
  }

  get unreadCount(): number {
    return this.notifications
      .filter(n => !n.isRead)
      .length;
  }
}
