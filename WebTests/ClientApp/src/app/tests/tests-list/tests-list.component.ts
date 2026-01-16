import { Component } from '@angular/core';
import { TestComponent } from '../tests/tests.component';
import { TestService, Test, Question, Option } from '../../services/test.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-tests-list',
  templateUrl: './tests-list.component.html',
  styleUrls: ['./tests-list.component.css']
})
export class TestsListComponent {
  tests: Test[] = [];

  constructor(
    private testService: TestService,
    private authService: AuthService,
  ) { }

  ngOnInit() {
    if (this.authService.isAuthenticated) {
      this.testService.getPublishedTests().subscribe({
        next: (data) => {
          this.tests = data;
        }
      })
    } else {
      this.testService.getPublishedTests().subscribe({
        next: (data) => {
          data.forEach((test) => {
            if (!test.types.includes("AuthOnly")) {
              this.tests.push(test);
            }
          })
        }
      })
    }
  }
}
