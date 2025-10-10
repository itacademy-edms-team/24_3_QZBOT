import { Component, OnInit } from '@angular/core';
import { TodoService, TodoItem } from '../services/todo.service';

@Component({
  selector: 'app-todo-list',
  templateUrl: './todo-list.component.html',
})
export class TodoListComponent implements OnInit {
  todos: TodoItem[] = [];

  constructor(private todoService: TodoService) { }

  ngOnInit(): void {
    this.todoService.getTodos().subscribe({
      next: (data) => {
        console.log('✅ Получены данные:', data);
        this.todos = data;
      },
      error: (err) => {
        console.error('❌ Ошибка при загрузке:', err);
      }
    });
  }

}
