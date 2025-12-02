import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NgForm } from '@angular/forms';

export interface RegisterRequest {
  username: string;
  email: string;
  phoneNumber: string;
  password: string;
}

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

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  onSubmit(form: NgForm) {
    if (this.model.password !== this.confirmPassword) {
      this.textError = 'Пароли не совпадают';
      return;
    }

    if (form.valid) {
      const registerData = {
        username: this.model.username,
        email: this.model.email,
        phoneNumber: this.model.phoneNumber,
        password: this.model.password,
      };

      this.authService.register(registerData).subscribe({
        next: () => this.router.navigate(['/login']),
        error: (err) => {
          console.error("Ошибка регистрации", err);
          this.textError = 'Ошибка регистрации. Проверьте введенные данные.';
        }
      });
    } else {
      console.log("Форма содержит ошибки");
      this.textError = 'Пожалуйста, заполните все обязательные поля.';
    }
  }
}
