import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  username: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    public authService: AuthService
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const name = params.get('name');

      if (name === "me") {
        const currentUser = this.authService.currentUserUsername;
        if (currentUser) {
          this.router.navigate(['/profile', currentUser], { replaceUrl: true });
        } else {
          this.router.navigate(['/login']);
        }
      } else {
        this.username = name || 'unknown';
      }
    });
  }

  onLogout() {
    this.authService.logout();
  }
}
