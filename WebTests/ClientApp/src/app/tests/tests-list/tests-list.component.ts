import { Component } from '@angular/core';
import { TestComponent } from '../tests/tests.component';
import { TestService, Test, Question, Option } from '../../services/test.service';
import { AuthService } from '../../services/auth.service';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-tests-list',
  templateUrl: './tests-list.component.html',
  styleUrls: ['./tests-list.component.css']
})
export class TestsListComponent {
  tests: Test[] = [];
  filteredTests: Test[] = [];
  searchText: string = '';
  isModalStartOpen: boolean = false;
  selectedTest: Test | null = null;

  constructor(
    private testService: TestService,
    private authService: AuthService,
    private router: Router,
  ) { }

  ngOnInit() {
    if (this.authService.isAuthenticated) {
      this.testService.getPublishedTests().subscribe({
        next: (data) => {
          this.tests = data;
          this.filteredTests = data;
        }
      })
    } else {
      this.testService.getPublishedTests().subscribe({
        next: (data) => {
          data.forEach((test) => {
            this.tests = data.filter(test => !test.types.includes("AuthOnly"));
            this.filteredTests = this.tests;
          })
        }
      })
    }
  }

  checkStart(test: Test) {
    this.testService.checkStart(test.id).subscribe({
      next: (data) => {
        if (data == "new test") {
          this.start(test);
        } else if (data == "continue test") {
          this.router.navigate(['/tests', test.id])
        } else if (data == "result") {
          this.router.navigate(['/results', test.id])
        }
      }
    })
  }

  start(test: Test) {
    this.selectedTest = test;
    this.isModalStartOpen = true;
  }

  closeModal() {
    this.isModalStartOpen = false;
    this.selectedTest = null;
  }

  filterTests() {
    const text = this.searchText.toLowerCase();

    this.filteredTests = this.tests.filter(test =>
      test.title.toLowerCase().includes(text)
    );
  }
}
