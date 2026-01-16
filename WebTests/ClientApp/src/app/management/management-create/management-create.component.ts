import { Component, OnInit } from '@angular/core';
import { TestService, Test, Question, TestType } from '../../services/test.service';

@Component({
  selector: 'app-management-create',
  templateUrl: './management-create.component.html',
  styleUrls: ['./management-create.component.css']
})
export class ManagementCreateComponent {

  // Модель теста
  test: Test = {
    id: 0,
    title: '',
    types: [],
    questions: [],
    creatorId: '',
    published: false,
    publishDate: new Date(0),
    createdDate: new Date(0),
    editDate: new Date(0),
    minimumSuccessPercent: 70,
  };

  types: TestType[] = [
    { id: 11, name: "Shuffle", description: "вопросы в случайном порядке" },
    { id: 12, name: "AuthOnly", description: "доступ только авторизованным" },
    { id: 13, name: "AllowBack", description: "возможность перемещения назад" },
    { id: 15, name: "ShowAfterEach", description: "показывать ответ сразу" },
    { id: 16, name: "ManyTimes", description: "возможно проходить несколько раз"}
  ];

  // Флаги для отображения UI
  is_exist: boolean = false;            // существует ли тест с таким названием
  confirm_add: boolean = false;         // показывать ли подтверждение
  success_add: boolean = false;         // успешно ли тест добавлен
  text_error: string = '';              // текст ошибки
  is_editing_locked: boolean = false;   // блокировка всех полей после подтверждения

  // Поля и флаги для JSON-импорта
  json_input: string = '';
  json_error: string = '';
  json_add: boolean = false;

  constructor(
    private testService: TestService
  ) { }


  ngOnInit() {
    // в будущем сделать получение типов из БД
  }

  // Добавление теста — первичная проверка данных
  btnAdd() {
    this.success_add = false;

    // проверка заполненности названия
    if (this.test.title.length == 0) {
      this.text_error = "Пустое поле названия";
      return;
    }
    if (this.test.minimumSuccessPercent > 100 || this.test.minimumSuccessPercent < 10) {
      this.text_error = "Недопустимое значение минимального процента ответов";
    }

    // проверяем у API, существует ли тест с таким названием
    this.testService.checkTestExists(this.test.title).subscribe({
      next: (data) => {
        this.is_exist = data;

        if (!this.is_exist) {

          const checkResult = this.testService.checkTest(this.test);

          if (checkResult == "true") {
            // показываем подтверждение и блокируем редактирование
            this.confirm_add = true;
            this.is_editing_locked = true;
            this.text_error = "";
          } else {
            // отображаем ошибку в форме
            this.text_error = checkResult;
          }

        } else {
          // ошибка - тест с таким названием уже существует
          this.text_error = "Такое название уже занято";
          this.confirm_add = false;
          this.success_add = false;
        }
      }
    })

    for (const q of this.test.questions) {
      const correctCount = q.options.filter(o => o.isCorrect).length;

      if (correctCount === 0) {
        this.text_error = "В каждом вопросе должен быть правильный вариант";
        return;
      }

      if (!q.isMultiple && correctCount > 1) {
        this.text_error = "В одиночном вопросе может быть только один правильный вариант";
        return;
      }
    }
  }

  // финальное подтверждение добавления теста
  btnConfirm() {
    // успешное добавление, изменяем состояние UI
    this.testService.addTest(this.test).subscribe({
      next: (data) => {
        if (data) {
          this.confirm_add = false;
          this.success_add = true;
          this.text_error = "";
          this.is_editing_locked = true;
        }
      }
    });

    //this.testService.addTest(this.test.title, this.test.questions).subscribe({
    //  next: (data) => {
    //    if (data) {
    //      this.confirm_add = false;
    //      this.success_add = true;
    //      this.text_error = "";
    //      this.is_editing_locked = true;
    //    }
    //  }
    //});
  }

  // отмена подтверждения - возвращаем возможность редактировать
  btnCancel() {
    this.confirm_add = false;
    this.is_editing_locked = false;
  }



  // ВОПРОСЫ

  // добавление нового пустого вопроса
  addQuestion() {
    this.test.questions.push({
      id: 0,
      text: '',
      options: [],
      isMultiple: false
    });
  }

  // удаление вопроса по индексу
  removeQuestion(index: number) {
    this.test.questions.splice(index, 1);
  }



  // ВАРИАНТЫ ОТВЕТОВ

  // добавление нового варианта к вопросу
  addOption(questions: Question) {
    questions.options.push({
      id: 0,
      text: '',
      isCorrect: false
    })
  }

  // удаление варианта из вопроса
  removeOption(question: Question, index: number) {
    question.options.splice(index, 1);
  }

  // указание типа теста
  toggleType(typeName: string) {
    const idx = this.test.types.indexOf(typeName);
    if (idx >= 0) {
      this.test.types.splice(idx, 1);
    } else {
      this.test.types.push(typeName);
    }
  }

  // выбор правильного варианта
  toggleCorrectOption(question: any, selectedIndex: number) {
    if (question.isMultiple) {
      question.options[selectedIndex].isCorrect = !question.options[selectedIndex].isCorrect
    } else {
      question.options.forEach((o: any, i: number) => {
        o.isCorrect = i === selectedIndex;
      })
    }
  }

  // множественный выбор
  onMultipleChange(question: any) {
    if (!question.isMultiple) {
      const firstCorrect = question.options.find((o: { isCorrect: any; }) => o.isCorrect);
      question.options.forEach((o: { isCorrect: boolean; }) => o.isCorrect = false);
      if (firstCorrect) {
        firstCorrect.isCorrect = true;
      }
    }
  }

  // JSON импорт теста
  loadFromJson() {
    this.json_error = '';

    try {
      const obj = JSON.parse(this.json_input); // в будущем расширить JSON

      // проверяем минимально корректную структуру 
      if (!obj.title || !Array.isArray(obj.questions)) {
        this.json_error = "Неправильная структура JSON";
        return;
      }

      // преобразуем JSON в типизированный объект Test
      this.test = {
        id: 0,
        title: obj.title,
        creatorId: '',
        types: obj.types.map((t: any) => { return t }),
        published: obj.published ?? false,
        questions: obj.questions.map((q: any) => ({
          id: 0,
          text: q.text,
          options: q.options.map((o: any) => ({
            id: 0,
            text: o.text,
            isCorrect: o.isCorrect
          }))
        })),
        publishDate: new Date(0),
        createdDate: new Date(0),
        editDate: new Date(0),
        minimumSuccessPercent: 70
      };

    } catch (e) {
      // JSON синтаксически неверный
      this.json_error = "Ошибка: невалидный JSON";
    }
  }
}
