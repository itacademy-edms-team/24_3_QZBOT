import { Component, OnInit } from '@angular/core';
import { Test, TestService } from '../../services/test.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-test-info',
  templateUrl: './test-info.component.html',
  styleUrls: ['./test-info.component.css']
})
export class TestInfoComponent implements OnInit {
  test: Test = {
    id: 0,
    title: '',
    questions: [],
    types: [],
    creatorId: '',
    published: false,
    publishDate: new Date(0),
    createdDate: new Date(0),
    editDate: new Date(0),
    minimumSuccessPercent: 70,
    coverUrl: '',
    description: '',
    difficult: 0,
  };

  constructor(
    private testService: TestService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const testId = Number(params.get('id'));
      if (testId) {
        this.testService.getTestById(testId).subscribe({
          next: (data) => {
            this.test = data;
          }
        })
      }
    })
  }
}
