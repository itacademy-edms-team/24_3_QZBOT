import { Component, OnInit, HostListener } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  isScrolled = false;
  isExpanded = false;
  currentUserUsername: string = '';

  constructor(
    public authService: AuthService,
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
}
