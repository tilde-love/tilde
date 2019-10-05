import {ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnDestroy, OnInit} from '@angular/core';
import {ProjectDataService} from '../../scripting/project-data.service';
import {isNullOrUndefined} from 'util';
import {BehaviorSubject, Subscription} from 'rxjs';
import {MatSlideToggleChange} from '@angular/material';
import {
  Control,
  ControlGroup,
  ControlType,
  Project,
  DataSourceType,
  DataSource,
  NumericRange
} from '../../scripting/_model/project-types';
import {DomSanitizer, SafeResourceUrl} from '@angular/platform-browser';

@Component({
  selector: 'app-control',
  templateUrl: './control.component.html',
  styleUrls: ['./control.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ControlComponent implements OnInit, OnDestroy {

  controlType = ControlType;

  @Input('project') public project: Project;
  @Input('controls') public controls: ControlGroup;
  @Input('control') public control: Control;

  public hasValidSource: boolean;
  public isReadonly: boolean;
  // public value: any;
  private _subscription: Subscription;

  // private _boolValue: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  // private _stringValue: BehaviorSubject<string> = new BehaviorSubject<string>('');
  // private _numberValue: BehaviorSubject<number> = new BehaviorSubject<number>(0);

//   import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
//
// srcData : SafeResourceUrl;
//
// constructor( private sanitizer: DomSanitizer ) { }
//
// this.srcData = this.sanitizer.bypassSecurityTrustResourceUrl(/* your base64 string in here*/);

  private _boolValue: boolean;
  private _stringValue: string;
  private _numberValue: number;
  private _uriValue: SafeResourceUrl;

  private _source: DataSource;
  private _possibleValues: string[] = []; // BehaviorSubject<string[]> = new BehaviorSubject<string[]>([]);
  private _range: NumericRange = { min: 0, max: 1, step: 1 };

  // public get boolValue() { return this._boolValue; }
  // public get stringValue() { return this._stringValue; }
  // public get numberValue() { return this._numberValue; }

  public get boolValue() { return this._boolValue; }
  public get stringValue() { return this._stringValue; }
  public get numberValue() { return this._numberValue; }
  public get uriValue() { return this._uriValue; }

  public get possibleValues() { return this._possibleValues; }
  public get range() { return this._range; }

  public value: any;

  public set boolValue(value: boolean) {
    // console.log('Set bool ' + value);
    this._boolValue = value;

    if (isNullOrUndefined(this._source)) {
      return;
    }

    this.projectDataService.updateControl(this.project.uri, this._source.uri, value);
  }

  public set numberValue(value: number) {
    // console.log('Set number ' + value);

    this._numberValue = value;

    if (isNullOrUndefined(this._source)) {
      return;
    }

    this.projectDataService.updateControl(this.project.uri, this._source.uri, value);
  }

  public set stringValue(value: string) {
    // console.log('Set string ' + value);

    this._stringValue = value;

    if (isNullOrUndefined(this._source)) {
      return;
    }

    this.projectDataService.updateControl(this.project.uri, this._source.uri, value);
  }

  // public boolChanged(event: MatSlideToggleChange) {
  //   event.
  // }
  //
  // public numberChanged(event: MatSlideToggleChange) {
  //   event.
  // }

  constructor(private projectDataService: ProjectDataService,
              private ref: ChangeDetectorRef,
              private sanitizer: DomSanitizer) { }

  ngOnInit() {


    if (isNullOrUndefined(this.controls)) {
      // console.log("No controls");
      return;
    }

    // console.log("controls: " + JSON.stringify(this.controls));
    //
    // if (isNullOrUndefined(this.controls.sources)) {
    //   console.log("No controls sources");
    //   return;
    // }

    if (isNullOrUndefined(this.control.source)) {
      // console.log("No source");
      return;
    }

    // console.log("this.project: " + JSON.stringify(this.project));


    if (this.project.controls.sources.hasOwnProperty(this.control.source) === false) {
      // console.log("No property");
      return;
    }

    if (isNullOrUndefined(this.project)) {
      return;
    }

    // // this.projectDataService.projects
    this._source = this.project.controls.sources[this.control.source];

    // console.log("got it");

    if (isNullOrUndefined(this._source)) {
      this.hasValidSource = false;
      this.isReadonly = true;

      return;
    }


    if (isNullOrUndefined(this._source.values)) {
      this._possibleValues = [];
    } else {
      this._possibleValues = this._source.values;
    }

    if (isNullOrUndefined(this.control.range) === false) {
      this._range = this.control.range;
    } else if (isNullOrUndefined(this._source.range) === false) {
      this._range = this._source.range;
    } else {
      this._range = { min: 0, max: 1, step: 1};
    }

    console.log(JSON.stringify(this.project.controls.values));

    if (isNullOrUndefined(this.project.controls.values) === false) {
      const initialValue = this.project.controls.values[this._source.uri];

      if (isNullOrUndefined(initialValue) === false) {
        this.updateValues(initialValue);
      }
    }

    this.isReadonly = this._source.readonly;

    this._subscription = this.projectDataService.attachControlEvent(this.project.uri, this._source.uri)
      .subscribe(event => {
        if (isNullOrUndefined(event)) {
          return;
        }

        this.updateValues(event.value);
      });

    this.ref.detectChanges();
  }

  private updateValues(value: any): void {
    switch (this._source.type) {
      case DataSourceType.Boolean:
        this._boolValue = value as boolean;
        this._stringValue = value.toString();
        break;
      case DataSourceType.Enum:
        this._stringValue = value.toString();
        break;
      case DataSourceType.String:
        this._stringValue = value as string;
        break;
      case DataSourceType.Float:
        this._numberValue = value as number;
        this._stringValue = value.toString();
        break;
      case DataSourceType.FloatArray:
        break;
      case DataSourceType.Integer:
        this._numberValue = value as number;
        this._stringValue = value.toString();
        break;
      case DataSourceType.IntegerArray:
        break;
      case DataSourceType.Color:
        this._stringValue = value.toString();
        break;
      case DataSourceType.Svg:
        this._uriValue = this.sanitizer.bypassSecurityTrustResourceUrl('none.png');
        this._uriValue = this.sanitizer.bypassSecurityTrustResourceUrl(value.toString());
        this._stringValue = value.toString();
        break;
      case DataSourceType.Image:
        break;
      case DataSourceType.Any:
        this._stringValue = JSON.stringify(value);
        break;
    }

    this.ref.detectChanges();
  }

  ngOnDestroy(): void {
    if (isNullOrUndefined(this._subscription) === false) { this._subscription.unsubscribe(); }
  }
}
