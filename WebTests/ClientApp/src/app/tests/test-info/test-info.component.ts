import { Component, OnInit } from '@angular/core';
import { Test, TestService } from '../../services/test.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, User } from '../../services/auth.service';
import { Observable } from 'rxjs';

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
    timeLimitSeconds: 0,
    isPublic: false,
    accessToken: ''
  };

  creator: User = {
    id: "",
    username: "",
    email: "",
    phoneNumber: "",
    avatarUrl: "",
    birthDate: new Date(0),
    status: ""
  }

  state: string = '';

  constructor(
    private testService: TestService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const testId = Number(params.get('id'));
      const testToken = String(params.get('token'));
      if (testId) {
        this.testService.getTestById(testId).subscribe({
          next: (data) => {
            this.test = data;
          },
          error: (err) => {
            console.error('Ошибка загрузки: ', err);
            this.router.navigate(['/tests'])
          }
        })

        this.testService.checkTestInfo(testId).subscribe({
          next: (data) => {
            this.state = data;
          }
        })

        this.testService.getAuthor(testId).subscribe({
          next: (data) => {
            this.creator = data;
          }
        })
      }

      else if (testToken) {
        this.testService.getTestByToken(testToken).subscribe({
          next: (data) => {
            this.test = data;
          },
          error: (err) => {
            console.error('Ошибка загрузки: ', err);
            this.router.navigate(['/tests'])
          }
        })

        this.testService.checkTestInfoByToken(testToken).subscribe({
          next: (data) => {
            this.state = data;
          }
        })

        this.testService.getAuthorByToken(testToken).subscribe({
          next: (data) => {
            this.creator = data;
          }
        })
      }
    })
  }

  getTestLink(test: Test | null | undefined) {
    if (!test) return ['/test'];

    return test.accessToken
      ? ['/tests', test.accessToken]
      : ['/tests/id', test.id];
  }
}
