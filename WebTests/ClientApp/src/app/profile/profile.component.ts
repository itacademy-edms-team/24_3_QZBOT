import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Test, TestService, UserTest } from '../services/test.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  username: string = '';
  history: UserTest[] = [];
  created_tests: Test[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    public authService: AuthService,
    public testService: TestService
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const name = params.get('name');

      if (name === "me") {
        this.authService.currentUser$.subscribe({
          next: (data) => {
            this.username = data || '';
          }
        })

        if (this.username) {
          this.router.navigate(['/profile', this.username], { replaceUrl: true });
        } else {
          this.router.navigate(['/login']);
        }
      } else {
        this.username = name || 'unknown';
      }
    });

    this.testService.getPassedTests().subscribe({
      next: (data) => {
        this.history = data;
      }
    })

    this.testService.getMyTests().subscribe({
      next: (data) => {
        this.created_tests = data;
      }
    })
  }

  onLogout() {
    this.authService.logout();
  }
}
