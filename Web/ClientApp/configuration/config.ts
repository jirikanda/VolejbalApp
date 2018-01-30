import { AspPrerenderData } from './ViewModels'

class ConfigSingleton {
    private static instanceInternal: ConfigSingleton;

    apiSettings: AspPrerenderData;
    dateFormat: string;
    inputDateFormat: string;
    dateTimeFormat: string;
    inputDateTimeFormat: string;

    constructor() {
        if (global && global['prerenderedData']) {
            this.apiSettings = global['prerenderedData'];
        }
        this.dateFormat = "DD.MM.YYYY";
        this.inputDateFormat = "YYYY-MM-DD";
        this.dateTimeFormat = "DD.MM.YYYY HH:mm";
        this.inputDateTimeFormat = "YYYY-MM-DD-HH-mm";
    };

    static get instance() {
        if (ConfigSingleton.instanceInternal) {
            return ConfigSingleton.instanceInternal;
        }
        ConfigSingleton.instanceInternal = new ConfigSingleton();
        return ConfigSingleton.instanceInternal;
    }
}

const configInstance = ConfigSingleton.instance;
export default configInstance;