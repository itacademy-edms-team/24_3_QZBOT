import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, User } from '../services/auth.service';
import { Test, TestPreview, TestService, UserTest } from '../services/test.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  is_user_exists: boolean = false;
  is_owner: boolean = false;
  name_from_url: string | null = '';
  name_for_subscribe: string = '';
  username: string = '';
  history: UserTest[] = [];
  created_tests: Test[] = [];
  liked_tests: TestPreview[] = [];
  user: User = {
    id: '',
    username: '',
    email: '',
    phoneNumber: '',
    avatarUrl: '',
    birthDate: new Date(),
    status: '',
    isFollowing: false
  }

  followSuccess: boolean = false;
  isLoadingFollow: boolean = false;
  showUnfollowModal = false;

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

      this.authService.getUserByUsername(this.name_from_url || '').subscribe({
        next: (data) => {
          this.user = data;
        }
      })

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

      this.testService.getLikedTests().subscribe({
        next: (data) => {
          this.liked_tests = data;
        }
      })

      if (this.name_from_url != null) {
        this.name_for_subscribe = this.name_from_url;
      }
    });
  }

  onLogout() {
    this.authService.logout();
  }

  startEdit(user: User) {
    this.router.navigate(['/editprofile', user])
  }

  follow() {
    if (this.user.isFollowing) {
      this.showUnfollowModal = true;
      return;
    }

    this.isLoadingFollow = true;

    this.authService.follow(this.name_for_subscribe).subscribe({
      next: () => {
        this.followSuccess = true;

        setTimeout(() => {
          this.user.isFollowing = true;
          this.followSuccess = false;
        }, 1200);

        this.isLoadingFollow = false;
      },

      error: () => {
        this.isLoadingFollow = false;
      }
    })
  }

  unfollow() {
    this.isLoadingFollow = true;

    this.authService.unfollow(this.user.username)
      .subscribe({
        next: () => {

          this.user.isFollowing = false;
          this.showUnfollowModal = false;
          this.isLoadingFollow = false;
        },

        error: () => {
          this.isLoadingFollow = false;
        }
      });
  }
}
