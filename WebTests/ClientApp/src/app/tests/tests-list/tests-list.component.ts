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

  sortBy: string = 'title';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private testService: TestService,
    private authService: AuthService,
    private router: Router,
  ) { }

  ngOnInit() {
    this.testService.getPublishedTests().subscribe({
      next: (data) => {
        if (this.authService.isAuthenticated) {
          this.tests = data;
        } else {
          this.tests = data.filter(t => !t.types.includes("AuthOnly"));
        }

        this.filteredTests = [...this.tests];
        this.applySorting();
      }
    });
  }

  checkStart(test: Test) {
    this.testService.checkTestInfo(test.id).subscribe({
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

    this.applySorting();
  }

  getTestLink(test: Test | null | undefined) {
    if (!test) return ['/test'];

    return test.accessToken
      ? ['/test/t', test.accessToken]
      : ['/test/id', test.id];
  }

  applySorting() {
    const sorted = [...this.filteredTests];

    sorted.sort((a, b) => {
      let result = 0;

      if (this.sortBy === 'title') {
        result = a.title.localeCompare(b.title);
      }

      if (this.sortBy === 'difficulty') {
        result = a.difficult - b.difficult;
      }

      return this.sortDirection === 'asc' ? result : -result;
    });

    this.filteredTests = sorted;
  }
}
