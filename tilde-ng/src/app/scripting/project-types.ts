export class ErrorSpan {
  public sl: number;
  public sc: number;
  public el: number;
  public ec: number;
}

export class Error {
  public type: string;
  public text: string;
  public span: ErrorSpan;
}

export enum ControlType {
  Toggle = 0,
  Checkbox,
  RadioButton,
  SelectBox,
  Chips,
  TextBox,
  Value,
  Slider,
  Graph,
  Svg,
  Color,
  Image,
  Markdown,
  Break,
}

export enum LayoutSize {
  Full = 0,
  Half = 1,
  Quarter = 4,
}

export enum DataSourceType {
  Boolean = 0,
  Enum,
  String,
  Float,
  FloatArray,
  Integer,
  IntegerArray,
  Color,
  Image,
  Graph,
  Svg,
  Any,
}

export class NumericRange {
  public min: number;
  public max: number;
  public step: number;
}

export class Control {
  public source: string;
  public type: ControlType;
  public size: LayoutSize;
  public label: string;
  public range: NumericRange;
}

export class ControlPanel {
  public controls: Control[] = [];
  public hash: string;
}

export class DataSource {
  public uri: string;
  public type: DataSourceType;
  public readonly: boolean;
  public range: NumericRange;
  public values: string[];
}

export class ControlGroup {
  public values: { [ uri: string]: any } = {};
  public panels: { [ uri: string]: ControlPanel } = {};
  public sources: { [ uri: string]: DataSource } = {};
}

export class Project {
  public uri: string;
  public deleted: boolean;
  public files: string[]; // { [name: string]: string };
  public errors: { [ uri: string]: Error[] } = {};
  public controls: ControlGroup;
}

export enum ScriptHostState {
  Running = 0,
  Stopped,
  Paused,
}

export class Runtime {
  public state: ScriptHostState;
  public isInError: boolean;
  public project: string;
}

export enum EditorState {
  NotCached = 0,
  NotFound,
  Cached,
  Edited,
  Superseded,
}

export class EditorItem {
  public state: EditorState;
  public uri: string;
  public mimeType: string;
  public original: string;
  public edited: string;
}

export class ControlEvent {
  public project: string;
  public control: string;
  public value: any;
}

export class GraphLine {
  public path: string;
  public color: string;
}
