import {Component, OnDestroy} from '@angular/core';
import {ProjectDataService} from './scripting/project-data.service';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnDestroy {
  title = 'app';
  private fileList: Array<File>;
  private _themeSubscription: Subscription;

  public get currentProject(): string {
    return this.projectDataService.runningProject;
  }

  constructor(private projectDataService: ProjectDataService) {
    this._themeSubscription = this.projectDataService.darkTheme.subscribe(dark => this.setTheme(dark));
  }

  setTheme(dark: boolean) {
    if (dark) {
      document.getElementById('main-body').classList.remove('app-light-theme');
      document.getElementById('main-body').classList.add('app-dark-theme');
    } else {
      document.getElementById('main-body').classList.remove('app-dark-theme');
      document.getElementById('main-body').classList.add('app-light-theme');
    }
  }

  onFilesChange(fileList: Array<File>) {
    this.fileList = fileList;

    if (this.fileList.length === 0) {
      return;
    }

    for (const file of this.fileList) {
      this.projectDataService.dropFile(file);
      // this.projectDataService.uploadProject(file);
    }

    // this.openUploadFilesDialog();
  }

  ngOnDestroy(): void {
    if (this._themeSubscription !== null) { this._themeSubscription.unsubscribe(); }
  }
}
