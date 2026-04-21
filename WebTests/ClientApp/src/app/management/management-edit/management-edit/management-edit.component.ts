import { Component, HostListener, OnInit, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { TestService, Test, Question, Option, TestType } from '../../../services/test.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { CdkDrag, CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { ComponentCanDeactivate } from '../../../validators/pending-changes.guard';
import { ImageCroppedEvent } from 'ngx-image-cropper';

@Component({
  selector: 'app-management-edit',
  templateUrl: './management-edit.component.html',
  styleUrls: ['./management-edit.component.css']
})
export class ManagementEditComponent implements OnInit, ComponentCanDeactivate {

  // Исходный тест, загруженный с сервера
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
  };

  // копия названия для отображения
  test_title: string = "";

  // копия редактируемого теста
  edited_test: Test = {
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
  };

  selectedCoverFile: File | null = null;     // выбранный файл обложки

  // переменные для обрезки изображения
  imageChangedEvent: any = '';               // событие изменения изображения для кроппера
  croppedImageBlob: Blob | null = null;
  croppedImagePreview: string | null = null;
  showCropper: boolean = false;              // показывать ли кроппер

  // сообщения и состояния UI
  text_error: string = "";
  confirm_edit: boolean = false;             // показывать окно подтверждения
  changes: string[] = [];                    // список изменений, вычисляется перед подтверждением 
  is_editing_locked: boolean = false;        // блокировка UI до подтверждения
  success_edit: boolean = false;             // флаг успешного сохранения

  currentUserId: string | null = null;       // userId

  isModalDeleting: boolean = false;          // модальное окно удаления теста
  isDeleted: boolean = false;                // информация об удалении теста

  types: TestType[] = [
    { id: 11, name: "Shuffle", description: "вопросы в случайном порядке" },
    { id: 12, name: "AuthOnly", description: "доступ только авторизованным" },
    { id: 13, name: "AllowBack", description: "возможность перемещения назад" },
    { id: 15, name: "ShowAfterEach", description: "показывать ответ сразу" },
    { id: 16, name: "ManyTimes", description: "возможно проходить несколько раз" }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private testService: TestService,
    private authService: AuthService,
  ) { }


  ngOnInit() {
    // получение userId
    this.authService.currentUserId.subscribe(id => {
      this.currentUserId = id;
    })

    // получение ID места из параметров маршрута
    this.route.paramMap.subscribe(params => {
      const test_id = Number(params.get('id'));
      if (test_id) {
        this.test.id = test_id;
      }

      // загрузка теста по ID 
      this.testService.getTestById(this.test.id).subscribe({
        next: (data) => {
          this.test = data;

          // проверка, что текущий пользователь - автор теста
          if (data.creatorId !== this.currentUserId) {
            alert("Вы не можете редактировать чужой тест");
            this.router.navigate(['/management']);
          }

          // создание копии объекта
          this.edited_test = JSON.parse(JSON.stringify(this.test));

          // сохраняем название для заголовка UI
          this.test_title = data.title;
        }
      });
    });
  }

  // добавление нового вопроса
  addQuestion() {
    this.edited_test.questions.push({
      id: 0,
      text: '',
      options: [],
      isMultiple: false,
    })
  }

  // удалить вопрос по индексу
  removeQuestion(index: number) {
    this.edited_test.questions.splice(index, 1);
  }

  // добавление нового варианта ответа
  addOption(questions: Question) {
    questions.options.push({
      id: 0,
      text: '',
      isCorrect: false
    });
  }

  // удаление варианта ответа
  removeOption(question: Question, index: number) {
    question.options.splice(index, 1);
  }

  // указание типа теста
  toggleType(typeName: string) {
    const idx = this.edited_test.types.indexOf(typeName);
    if (idx >= 0) {
      this.edited_test.types.splice(idx, 1);
    } else {
      this.edited_test.types.push(typeName);
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

  // кнока "сохранить изменения"
  btnSaveChanges() {
    // перед сохранением проверяем корректность теста
    const validation = this.testService.checkTest(this.edited_test);

    if (validation == "true") {

      // получем список изменений между исходным тестом и измененным
      this.changes = this.testService.getTestChanges(this.test, this.edited_test);

      this.testService.checkTestExists(this.edited_test.title).subscribe({
        next: (data) => {
          if (data) {
            if (!(this.test.title === this.edited_test.title)) {
              this.text_error = "Такое название теста уже занято"
              return;
            }

            // если изменений нет - выводим сообщение
            if (this.changes.length == 0) {
              this.text_error = "Нет изменений";
              return;
            }

            // если есть - блокируем редактирование и показываем окно подтверждения
            this.is_editing_locked = true;
            this.confirm_edit = true;
            this.text_error = "";
          }
        }
      })

    } else {
      // ошибка валидации теста - показывем пользователю
      this.text_error = validation;
    }
  }

  // отмена окна подтверждения
  btnCancel() {
    this.confirm_edit = false;
    this.is_editing_locked = false;
  }

  // подтверждение и отправка изменений на сервер
  btnConfirm() {
    const formData = new FormData();

    formData.append("test", JSON.stringify(this.edited_test));

    if (this.selectedCoverFile) {
      formData.append("cover", this.selectedCoverFile);
    }

    this.testService.editTest(this.test.id, formData).subscribe({
      next: (data) => {
        if (data) {
          // успех
          this.success_edit = true;
          this.is_editing_locked = false;
          this.text_error = "";
        }
        else {
          // ошибка сервера
          this.text_error = "Ошибка редактирования теста"
        }
      }
    })
  }

  // закрытие модального окна
  closeModal() {
    this.isModalDeleting = false;
    this.isDeleted = false;
  }

  // закрытие окна, информирующего об удалении
  closeInfoModal() {
    this.isDeleted = false;
    this.router.navigate(['/management/edit']);
  }

  // удаление теста
  deleteTest() {
    this.isModalDeleting = true;
  }

  // подтверждение удаления
  confirmDelete() {
    this.isModalDeleting = false

    this.testService.deleteTest(this.test.id).subscribe({
      next: (data) => {
        if (data) {
          this.isDeleted = true;
        } else {
          
        }
      }
    })
  }


  // перетаскивание вопросов и вариантов ответов
  dropQuestion(event: CdkDragDrop<typeof this.edited_test.questions>) {
    moveItemInArray(this.edited_test.questions, event.previousIndex, event.currentIndex);
  }

  dropOption(event: CdkDragDrop<any[]>, questionIndex: number) {
    moveItemInArray(this.edited_test.questions[questionIndex].options, event.previousIndex, event.currentIndex);
  }


  // защита от перехода на другую страницу при изменении данных
  canDeactivate(): boolean {
    if (this.success_edit) {
      return true;
    }

    const isChanged = JSON.stringify(this.test) !== JSON.stringify(this.edited_test);

    if (isChanged) {
      return confirm("У вас есть несохраненные изменения. Вы уверены, что хотите уйти?")
    }
    return true;
  }

  // защита от закрытия вкладки при изменении данных
  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    const isChanged = JSON.stringify(this.test) !== JSON.stringify(this.edited_test);

    if (isChanged) {
      $event.returnValue = true;
    }
  }

  // получение доступа к элементу input для загрузки обложки
  @ViewChild('coverInput') coverInputVariable!: ElementRef;

  // обработка выбора файла обложки
  onCoverSelected(event: any) {
    this.imageChangedEvent = event;

    this.showCropper = true;
  }

  imageCropped(event: ImageCroppedEvent) {
    if (event.blob) {
      this.croppedImageBlob = event.blob;

      this.croppedImagePreview = URL.createObjectURL(event.blob);
    }
  }

  // сброс выбранного файла в input после кропа или отмены
  resetInput() {
    this.coverInputVariable.nativeElement.value = "";
  }

  applyCrop() {
    if (this.croppedImageBlob) {
      this.selectedCoverFile = new File(
        [this.croppedImageBlob],
        "cover.jpg",
        { type: "image/jpeg" }
      );

      this.edited_test.coverUrl = this.croppedImagePreview || '';

      this.showCropper = false;
      this.imageChangedEvent = "";

      this.resetInput();
    }
  }

  // отмена обрезки изображения
  cancelCrop() {
    this.showCropper = false;
    this.imageChangedEvent = '';
    this.resetInput();
  }
}
