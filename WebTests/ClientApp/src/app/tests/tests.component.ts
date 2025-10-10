import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TestService, Test } from '../services/test.service';

@Component({
  selector: 'app-test',
  templateUrl: './tests.component.html',
  styleUrls: ['./tests.component.css']
})
export class TestComponent implements OnInit {

  tests: Test[] = [];
  errorMessage = '';

  constructor(
    private testService: TestService,
    private route: ActivatedRoute,
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const testName = params.get('name');
      if (testName) {
        this.loadTests(testName);
      }
    });
  }

  loadTests(name: string) {
    this.testService.getTestsByName(name).subscribe({
      next: data => this.tests = data,
      error: err => console.error(err)
    });
  }
}
