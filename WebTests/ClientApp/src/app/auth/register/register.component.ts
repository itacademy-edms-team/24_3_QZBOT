import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  model = {
    username: '',
    email: '',
    phoneNumber: '',
    password: '',
  };
  confirmPassword: string = '';
  textError: string = '';

  constructor(private authService: AuthService, private router: Router) { }

  onSubmit() {
    if (this.confirmPassword != this.model.password) {
      this.textError = "Пароли не совпадают"
    }

    this.authService.register(this.model).subscribe({
      next: () => this.router.navigate(['/login']),
      error: (err) => {
        console.error("Ошибка регистрации: ", err);
        this.textError = err;
      }
    })
  }
}
