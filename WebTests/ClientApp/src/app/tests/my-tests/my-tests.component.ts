import { Component } from '@angular/core';
import { Test, TestService } from '../../services/test.service';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';


@Component({
  selector: 'app-my-tests',
  templateUrl: './my-tests.component.html',
  styleUrls: ['./my-tests.component.css']
})
export class MyTestsComponent {
  tests: Test[] = []

  constructor(
    private testService: TestService,
    private router: Router,
    private route: ActivatedRoute,
  ) { }

  ngOnInit() {
    this.testService.getMyTests().subscribe({
      next: (data) => this.tests = data
    });
  }
}
