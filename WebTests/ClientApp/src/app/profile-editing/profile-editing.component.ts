import { Component, OnInit } from '@angular/core';
import { AuthService, User } from '../services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { TestService } from '../services/test.service';

@Component({
  selector: 'app-profile-editing',
  templateUrl: './profile-editing.component.html',
  styleUrls: ['./profile-editing.component.css']
})
export class ProfileEditingComponent implements OnInit {
  edit_access: boolean = false;
  username: string = "";
  user: User = {
    id: '',
    username: '',
    email: '',
    phoneNumber: '',
    avatarUrl: '',
    birthDate: new Date(),
    status: ''
  };
  formattedBirthDate: string = "";

  edited_user: User = {
    id: '',
    username: '',
    email: '',
    phoneNumber: '',
    avatarUrl: '',
    birthDate: new Date(),
    status: ''
  }
  editedFormattedBirthDate: Date = new Date();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    public authService: AuthService,
    public testService: TestService
  ) { }

  ngOnInit() {
    this.authService.currentUser$.subscribe({
      next: (data) => {
        this.username = data || '';

        this.authService.getUserByUsername(this.username).subscribe({
          next: (data) => {
            this.user = data;
            //this.formattedBirthDate = new Date(this.user.birthDate).toISOString().split(`T`)[0];

            this.edited_user = JSON.parse(JSON.stringify(this.user));

          }
        })
      }
    })
  }

  btnSaveChanges() {
    this.authService.editUserProfile(this.user.username, this.edited_user).subscribe({
      next: (data) => {
        if (data) {
          this.edit_access = true;
        }
      }
    })
  }
}
