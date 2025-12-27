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
  errors: string[] = [];

  isSuccessModalOpen: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  onSubmit(form: NgForm) {
    if (form.valid) {
      const registerData = {
        username: this.model.username,
        email: this.model.email,
        phoneNumber: this.model.phoneNumber,
        password: this.model.password,
      };

      this.authService.register(registerData).subscribe({
        next: () => {
          this.isSuccessModalOpen = true;
        }, 
        error: (err) => {
          this.errors = [];

          if (err.error.code == "DuplicateUserName") {
            this.errors.push("Это пользователя уже занято")
            return;
          }

          if (err.error.code == "DuplicateEmail") {
            this.errors.push("Эта почта уже занята")
            return;
          }

          for (var i = 0; i < err.error.length; i++) {
            if (err.error[i].code == "PasswordTooShort") {
              this.errors.push("Пароль должен содержать не менее 6 символов");
            } else if (err.error[i].code == "PasswordRequiresNonAlphanumeric") {
              this.errors.push("Пароль должен содержать хотя бы один спецсимвол");
            } else if (err.error[i].code == "PasswordRequiresLower") {
              this.errors.push("Пароль должен содержать хотя бы одну букву нижнего регистра");
            } else if (err.error[i].code == "PasswordRequiresUpper") {
              this.errors.push("Пароль должен содержать хотя бы одну букву верхнего регистра");
            } else if (err.error[i].code == "PasswordRequiresDigit") {
              this.errors.push("Пароль должен содержать хотя бы одну цифру")
            }
          }

          console.error("Ошибка регистрации", err);
        }
      });
    } else {
      console.log("Форма содержит ошибки");
    }
  }

  closeModal() {
    this.isSuccessModalOpen = false;
    this.router.navigate(['/login'])
  }
}
