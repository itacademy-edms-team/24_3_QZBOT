import { Component, OnInit } from '@angular/core';
import { Test, TestInfo, TestService } from '../../services/test.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, User } from '../../services/auth.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-test-info',
  templateUrl: './test-info.component.html',
  styleUrls: ['./test-info.component.css']
})
export class TestInfoComponent implements OnInit {

  test: TestInfo = {
      id: 0,
      title: '',
      questions: [],
      types: [],
      creatorId: '',
      publishDate: new Date(0),
      createdDate: new Date(0),
      editDate: new Date(0),
      minimumSuccessPercent: 70,
      coverUrl: '',
      description: '',
      difficult: 0,
      timeLimitSeconds: 0,
      accessToken: '',
      likesCount: 0,
      isLiked: false,
      isSaved: false
  }

  creator: User = {
    id: "",
    username: "",
    email: "",
    phoneNumber: "",
    avatarUrl: "",
    birthDate: new Date(0),
    status: "",
    isFollowing: false
  }

  state: string = '';
  isAuth: boolean = this.authService.isAuthenticated;
  isOwner: boolean = false;

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
        this.testService.getTestInfoById(testId).subscribe({
          next: (data) => {
            this.test = data;
            //this.isOwner = this.test.creatorId === this.authService.currentUserId;
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
        this.testService.getTestInfoByToken(testToken).subscribe({
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

  getTestLink(test: TestInfo | null | undefined) {
    if (!test) return ['/test'];

    return test.accessToken
      ? ['/tests/t', test.accessToken]
      : ['/tests/id', test.id];
  }

  likeTest() {
    if (!this.test.isLiked) {
      this.testService.likeTest(this.test.id).subscribe({
        next: () => {
          console.log(`Liked test with ID: ${this.test.id}`);
        }
      })
    } else {
      this.testService.unlikeTest(this.test.id).subscribe({
        next: () => {
          console.log(`Unliked test with ID: ${this.test.id}`);
        }
      })
    }
  }

  saveTest() {
    if (!this.test.isSaved) {
      this.testService.saveTest(this.test.id).subscribe({
        next: () => {
          console.log(`Saved test with ID: ${this.test.id}`);
        }
      })
    } else {
      this.testService.unsaveTest(this.test.id).subscribe({
        next: () => {
          console.log(`Unsaved test with ID: ${this.test.id}`);
        }
      })
    }
  }
}
