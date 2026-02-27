import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { TestService, UserTest } from '../services/test.service';

@Component({
  selector: 'app-results',
  templateUrl: './results.component.html',
  styleUrls: ['./results.component.css']
})

export class ResultsComponent {
  test_id: number = 0;
  userTests: UserTest[] = [];
  manyTimes: boolean = false;
  dateDiff: Date = new Date();

  type = {
    id: 16,
    name: "ManyTimes",
    description: "возможно проходить несколько раз"
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private testService: TestService,
  ) { }
  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (id) {
        this.test_id = id;
      }

      this.testService.getAttempts(this.test_id).subscribe({
        next: (data) => {
          this.userTests = data;

          //this.dateDiff = this
        }
      })
    })
  }
}
