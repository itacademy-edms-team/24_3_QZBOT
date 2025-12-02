import { Component } from '@angular/core';
import { NgForm, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: 'login.component.html',
  styleUrls: ['login.component.css']
})
export class LoginComponent {
  model = {
    email: '',
    password: ''
  };

  constructor(private authService: AuthService, private router: Router) { }

  onSubmit(form: NgForm) {
    if (form.valid) {
      this.authService.login(this.model).subscribe({
        next: () => { },
        error: (err) => console.error('Ошибка входа: ', err)
      });
    } else {
      console.log('Форма содержит ошибки');
    }
  }
}
