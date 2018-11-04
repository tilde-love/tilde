import { ScriptingModule } from './scripting.module';

describe('ScriptingModule', () => {
  let scriptingModule: ScriptingModule;

  beforeEach(() => {
    scriptingModule = new ScriptingModule();
  });

  it('should create an instance', () => {
    expect(scriptingModule).toBeTruthy();
  });
});
