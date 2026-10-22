import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { TestPreview, TestService } from '../../services/test.service';

@Component({
  selector: 'app-saved-tests',
  templateUrl: './saved-tests.component.html',
  styleUrls: ['./saved-tests.component.css']
})
export class SavedTestsComponent {
  tests: TestPreview[] = [];

  constructor(
    private testService: TestService,
    private authService: AuthService,
    private router: Router,
  ) { }

  ngOnInit() {
    this.testService.getSavedTests().subscribe({
      next: (data) => {
        this.tests = data;
      }
    })
  }

  getTestLink(test: TestPreview | null | undefined) {
    if (!test) return ['/test'];

    return test.accessToken
      ? ['/test/t', test.accessToken]
      : ['/test/id', test.id];
  }
}
