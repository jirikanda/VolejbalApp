import * as React from 'react';
import { Input } from 'reactstrap';
import * as moment from 'moment';
import config from '../../configuration/config';

const InputFormField = ({ input, disabled, label, type, children }) => {
    if (type === "date" && Date.parse(input.value)) {
        input.value = moment(input.value).format(config.inputDateTimeFormat);
    }

    return <div className="form-group">
        <Input {...input} type={type} disabled={disabled}>
            {children}
        </Input>
    </div>;
}

export default InputFormField