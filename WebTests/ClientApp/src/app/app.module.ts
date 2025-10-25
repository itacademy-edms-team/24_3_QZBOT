import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
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
    ManagementEditListComponent
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
      { path: 'management/edit/:title', component: ManagementEditComponent }
    ]),
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
