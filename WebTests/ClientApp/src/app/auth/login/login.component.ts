import { Component } from '@angular/core';
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

  onSubmit() {
    console.log("Нажимается")
    this.authService.login(this.model).subscribe({
      next: () => { },
      error: (err) => console.error('Ошибка входа: ', err)
    });
  }
}
