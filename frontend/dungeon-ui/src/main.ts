import { Aurelia } from 'aurelia-framework';
import { PLATFORM } from 'aurelia-pal';

export function configure(aurelia: Aurelia) {
  aurelia.use
    .standardConfiguration()
    //.feature(PLATFORM.moduleName('resources/index'));

  // Hardcoded flags (instead of environment.json)
  const debug = true;
  const testing = false;

  aurelia.use.developmentLogging(debug ? 'debug' : 'warn');

  if (testing) {
    aurelia.use.plugin(PLATFORM.moduleName('aurelia-testing'));
  }

  aurelia.start().then(() => aurelia.setRoot(PLATFORM.moduleName('app')));
}
