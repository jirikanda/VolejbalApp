import { actions, toastr } from "react-redux-toastr";

export interface ToasterOptions {
    message: string;
    title?: string;
    showCloseButton?: boolean;
    additionalOptions?: {
        timeOut?: number;   // how long toastr will be displayed
        onShowComplete?: () => void;
        onHideComplete?: () => void;
        onCloseButtonClick?: () => void;
    }
}

export interface ToastrConfirmOptions {
    message: string;
    onOk?: () => void;
    onCancel?: () => void;
};

const createSettings = (defaultSettings: ToasterOptions) => (optionalSettings: ToasterOptions) => {
	return { ...defaultSettings, ...optionalSettings };
}

const creatSettingsWithDefault = createSettings({
    message: "",
    showCloseButton: false,
    additionalOptions: { timeOut: 5000}
});

export const Toastr = {
    success(options: ToasterOptions) {
        const { title, message, additionalOptions }: ToasterOptions = creatSettingsWithDefault({  title: 'Info', ...options });
        toastr.success(title, message, additionalOptions);
    },
    error(options: ToasterOptions) {
        const { title, message, additionalOptions }: ToasterOptions = creatSettingsWithDefault({  title: 'Chyba', ...options });
        toastr.error(title, message, additionalOptions);
    },
    confirm(options: ToastrConfirmOptions) {
        const { message, onOk, onCancel } = options;
        toastr.confirm(message, {onOk, onCancel});
    }
}