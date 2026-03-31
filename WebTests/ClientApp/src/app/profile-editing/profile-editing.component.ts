import { Component, HostListener, OnInit } from '@angular/core';
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

  selectedFile: File | null = null;

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

            this.edited_user = JSON.parse(JSON.stringify(this.user));

          }
        })
      }
    })
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.selectedFile = file;

      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.edited_user.avatarUrl = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  removeAvatar() {
    this.edited_user.avatarUrl = '';
  }

  btnSaveChanges() {
    const formData = new FormData();

    formData.append('status', this.edited_user.status);
    formData.append('birthDate', this.edited_user.birthDate.toString());

    if (this.selectedFile) {
      formData.append('avatar', this.selectedFile);
    } else {
      formData.append('avatar', '');
    }

    this.authService.editUserProfile(this.user.username, formData).subscribe({
      next: (data) => {
        if (data) {
          this.edit_access = true;
        }
      }
    })
  }


  // защита от перехода на другую страницу при изменении данных
  canDeactivate(): boolean {
    if (this.edit_access) {
      return true;
    }

    const isChanged = JSON.stringify(this.user) !== JSON.stringify(this.edited_user);

    if (isChanged) {
      return confirm("У вас есть несохраненные изменения. Вы уверены, что хотите уйти?")
    }

    return true;
  }

  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    //if (this.edit_access) {
    //  $event.returnValue = false;
    //}

    const isChanged = JSON.stringify(this.user) !== JSON.stringify(this.edited_user);

    if (isChanged) {
      $event.returnValue = true;
    }
  }
}
