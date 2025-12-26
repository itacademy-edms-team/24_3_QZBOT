import { Component, OnInit } from '@angular/core';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  title = 'app';
  constructor(public authService: AuthService) { }

  ngOnInit() {
    this.authService.checkAuth().subscribe({
      error: () => { }
    })
  }
}
