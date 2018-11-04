import {Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild} from '@angular/core';
import {AceEditorComponent} from 'ng2-ace-editor';
import {Error, ProjectDataService} from '../project-data.service';
import {Subscription} from 'rxjs';
// import {Ace} from 'ace-builds';
// import Point = Ace.Point;
// import Range = Ace.Range;
// declare let ace: any;

@Component({
  selector: 'app-script-editor',
  templateUrl: './script-editor.component.html',
  styleUrls: ['./script-editor.component.scss']
})
export class ScriptEditorComponent implements OnInit, OnDestroy {

  @ViewChild('editor') editor;
  private _themeSubscription: Subscription;

  @Input('text')
  public get text(): string {
    return this._text;
  }
  public set text(text: string) {
    this._text = text;
  }

  @Input('mode')
  public get mode(): string {
    return this._mode;
  }
  public set mode(mode: string) {
    this._mode = mode;
  }

  @Input('errors')
  public get errors(): Error[] {
    return this._projectErrors;
  }
  public set errors(projectErrors: Error[]) {
    // console.log('set errors');
    this._projectErrors = projectErrors;
    this.setErrorHighlighting();
    this.editor.getEditor().setReadOnly(this.readonly);
  }

  @Input('readonly')
  public get readonly(): boolean {
    return this._readonly;
  }

  public set readonly(readonly: boolean) {
    this._readonly = readonly;

    this.editor.getEditor().setOptions({
      enableBasicAutocompletion: true,
      fontSize: '14pt',
      maxLines: 4096,
      behavioursEnabled: true,
      wrapBehavioursEnabled: true,
      showPrintMargin: true,
      showGutter: !this.readonly,
      indentedSoftWrap: true,
      wrap: true,
      readOnly: this.readonly
    });
  }

  public theme: string = 'tomorrow_night_bright';

  private _mode: string = '';
  private _readonly: boolean = false;
  private _text: string = '';
  private _projectErrors: Error[] = [];
  private _textChanged: EventEmitter<{}> = new EventEmitter<{}>();

  @Output() public get textChanged(): EventEmitter<{}> {
    // console.log("editor " + this.editor);
    // console.log("textChanged " + this.editor.textChanged);

    return this._textChanged;
  }

  constructor(private projectDataService: ProjectDataService) {
    this._themeSubscription = this.projectDataService.darkTheme.subscribe(dark => this.setTheme(dark));
  }

  setTheme(dark: boolean) {
    if (dark) {
      this.theme = 'tomorrow_night_bright';
    } else {
      this.theme = 'textmate';
    }
  }

  onChange(editor) {
    this._textChanged.emit(this.text);
    this.setErrorHighlighting();
  }

  ngOnInit(): void {
    // console.log(`readonly: ${this.readonly}`);

    this.editor.getEditor().setOptions({
      enableBasicAutocompletion: true,
      fontSize: '14pt',
      maxLines: 4096,
      behavioursEnabled: true,
      wrapBehavioursEnabled: true,
      showPrintMargin: true,
      showGutter: !this.readonly,
      indentedSoftWrap: true,
      wrap: true,
      readOnly: this.readonly
    });
    this.editor.getEditor().getSession().setUseWrapMode(true);
    this.editor.getEditor().getSession().setOption('useWorker', false);
    this.editor.getEditor().setReadOnly(this.readonly);
    // this.editor.getEditor().setShowGutter(!this.readonly);
    // // This array holds all the errors
    // const jsonErrorArray = [];
    // const errorLinesArray = [1, 5, 7];
    // const errorMessagesArray = ['Error on line 1', 'Error on line 5', 'Error on line 7'];
    //
    // for (let i = 0; i < errorLinesArray.length; i++) {
    //   jsonErrorArray.push({
    //     row: errorLinesArray[i] - 1,
    //     column: 10,
    //     type: 'error',
    //     text: errorMessagesArray[i]
    //   });
    // }
    //
    // // Set the annotations to the editor
    // this.editor.getEditor().getSession().setAnnotations(jsonErrorArray);

    // ace.config.set("workerPath", "assets/js/plugins/editors/ace/")

    this.editor.getEditor().commands.addCommand({
      name: 'showOtherCompletions',
      bindKey: 'Ctrl-.',
      exec: function (editor) {
      }
    });
  }

  private setErrorHighlighting() {

    // console.log(JSON.stringify(this._projectErrors));
    // this.editor.getEditor().getSession().C('errors');

    // This array holds all the errors
    const jsonErrorArray = [];
    const jsonMarkersArray = [];

    for (let i = 0; i < this._projectErrors.length; i++) {
      const error = this._projectErrors[i];

      jsonErrorArray.push({
        row: error.span.sl,
        column: error.span.sc,
        type: error.type,
        text: error.text
      });

      const start = { row: error.span.sl, column: error.span.sc };
      const end = { row: error.span.el, column: error.span.ec };

      // this.editor.getEditor().getSession().addMarker(new Range(error.line, error.column, error.line, error.column + error.length), 'error-highlight');
      // this.editor.getEditor().getSession().addMarker(new Range(0, 0, 2, 1), 'error-highlight', 'fullLine');
      // markers.push({startRow: error.line, startCol: error.column, endRow: error.line, endCol: error.column + error.length, className: 'error-highlight', type: 'error' });

      // this.editor.getEditor().getSession().addMarker({start: start, end: end});
    }

    // Set the annotations to the editor
    this.editor.getEditor().getSession().setAnnotations(jsonErrorArray);

    // console.log('Text changed');
    // console.log(this.text);
  }

  ngOnDestroy(): void {
    if (this._themeSubscription !== null) { this._themeSubscription.unsubscribe(); }
  }
}
