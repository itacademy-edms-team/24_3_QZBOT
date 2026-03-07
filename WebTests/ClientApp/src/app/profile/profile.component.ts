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
  is_user_exists: boolean = false;
  is_owner: boolean = false;
  name_from_url: string | null = '';
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
      this.name_from_url = params.get('name');

      if (this.name_from_url === "me") {
        this.authService.currentUser$.subscribe({
          next: (data) => {
            this.username = data || '';
          }
        })

        if (this.username == '') {
          this.router.navigate(['/login']);
          this.is_owner = false;
        } else {
          this.is_owner = true;
        }

      } else {
        this.authService.currentUser$.subscribe({
          next: (data) => {
            this.username = data || '';
          }
        })

        if (this.name_from_url != null) {
          this.authService.isUserExist(this.name_from_url).subscribe({
            next: (exist) => {
              if (exist) {
                this.is_user_exists = true;
              }
            }
          })
        }

        if (this.username == this.name_from_url) {
          this.is_owner = true;
        } else {
          this.is_owner = false;
        }
      }
    });

    this.testService.getPassedTestsByUsername(this.name_from_url).subscribe({
      next: (data) => {
        this.history = data;
      }
    })

    this.testService.getMyTestsByUsername(this.name_from_url).subscribe({
      next: (data) => {
        this.created_tests = data;
      }
    })
  }

  onLogout() {
    this.authService.logout();
  }
}
