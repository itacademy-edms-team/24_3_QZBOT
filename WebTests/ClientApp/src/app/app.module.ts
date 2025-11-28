import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AuthInterceptor } from './auth/auth.interceptor';

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
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'tests/:name', component: TestComponent },
      { path: 'tests', component: TestsListComponent },

      { path: 'management', component: ManagementComponent },
      { path: 'management/create', component: ManagementCreateComponent },
      { path: 'management/edit', component: ManagementEditListComponent },
      { path: 'management/edit/:id', component: ManagementEditComponent },

      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent },

      { path: 'profile', redirectTo: '/profile/me', pathMatch: 'full' },
      { path: 'profile/:name', component: ProfileComponent, canActivate: [authGuard] }
    ]),
    AppRoutingModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
