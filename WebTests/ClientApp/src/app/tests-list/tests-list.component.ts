import { Component } from '@angular/core';
import { TestComponent } from '../tests/tests.component';
import { Test } from '../services/test.service';

@Component({
  selector: 'app-tests-list',
  templateUrl: './tests-list.component.html',
  styleUrls: ['./tests-list.component.css']
})
export class TestsListComponent {
  tests: Test[] = [];

   //здесь можно сделать автоматическое получение тестов и вывод на
   //страницу списком, но их по сути неоткуда брать с помощью цикла
}
