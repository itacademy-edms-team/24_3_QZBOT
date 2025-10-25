import { Component, OnInit } from '@angular/core';
import { TestService, Test } from '../../../services/test.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-management-edit-list',
  templateUrl: './management-edit-list.component.html',
  styleUrls: ['./management-edit-list.component.css']
})
export class ManagementEditListComponent {
  tests: Test[] = [];
  title!: string

  constructor(
    private testService: TestService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
  ) { }

  ngOnInit() {
    this.testService.getAllTests().subscribe({
      next: (data) => {
        this.tests = data;
      }
    })
  }
}
