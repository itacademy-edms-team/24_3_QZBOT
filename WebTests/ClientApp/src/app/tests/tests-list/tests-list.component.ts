import { Component } from '@angular/core';
import { TestComponent } from '../tests/tests.component';
import { TestService, Test, Question, Option } from '../../services/test.service';

@Component({
  selector: 'app-tests-list',
  templateUrl: './tests-list.component.html',
  styleUrls: ['./tests-list.component.css']
})
export class TestsListComponent {
  tests: Test[] = [];

  constructor(
    private testService: TestService,
  ) { }

  ngOnInit() {
    this.testService.getAllTests().subscribe({
      next: (data) => {
        this.tests = data;
      }
    })
  }
}
