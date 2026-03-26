import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { AppRoutingModule } from './app-routing.module';
import { TestComponent } from './tests/tests/tests.component';
import { TestsListComponent } from './tests/tests-list/tests-list.component';
import { ManagementComponent } from './management/management.component';
import { ManagementCreateComponent } from './management/management-create/management-create.component';
import { ManagementEditComponent } from './management/management-edit/management-edit/management-edit.component';
import { ManagementEditListComponent } from './management/management-edit/management-edit-list/management-edit-list.component';
import { ProfileComponent } from './profile/profile.component';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';
import { authGuard } from './auth/auth.guard';
import { MyTestsComponent } from './tests/my-tests/my-tests.component';
import { ResultsComponent } from './results/results.component';
import { CdkDrag, DragDropModule } from '@angular/cdk/drag-drop';
import { ProfileEditingComponent } from './profile-editing/profile-editing.component';
import { pendingChangesGuard } from './validators/pending-changes.guard';
import { ImageCropperModule } from 'ngx-image-cropper';
import { CommonModule } from '@angular/common';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    TestComponent,
    TestsListComponent,
    ManagementComponent,
    ManagementCreateComponent,
    ManagementEditComponent,
    ManagementEditListComponent,
    ProfileComponent,
    LoginComponent,
    RegisterComponent,
    MyTestsComponent,
    ResultsComponent,
    ProfileEditingComponent,
  ],
  imports: [
    CommonModule,
    ImageCropperModule,
    DragDropModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'tests/:id', component: TestComponent },
      { path: 'tests', component: TestsListComponent },

      { path: 'management', component: ManagementComponent },
      { path: 'management/create', component: ManagementCreateComponent, canDeactivate: [pendingChangesGuard] },
      { path: 'management/edit', component: ManagementEditListComponent },
      { path: 'management/edit/:id', component: ManagementEditComponent, canDeactivate: [pendingChangesGuard] },

      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent },

      { path: 'profile', redirectTo: '/profile/me', pathMatch: 'full' },
      { path: 'profile/:name', component: ProfileComponent },

      { path: 'editprofile', component: ProfileEditingComponent, canActivate: [authGuard], canDeactivate: [pendingChangesGuard] },

      { path: 'my-tests', component: MyTestsComponent, canActivate: [authGuard] },

      { path: 'results/:id', component: ResultsComponent, canActivate: [authGuard] }
    ]),
    AppRoutingModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
