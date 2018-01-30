import * as React from 'react';
import { Input, InputGroup, InputGroupAddon, FormFeedback } from 'reactstrap';
import * as moment from 'moment';
import config from '../../configuration/config';
import * as classnames from 'classnames';

const ValidatedInputFormField = ({ input, label, type, meta: { touched, error, invalid, valid }, children, disabled, id }) => {
    let classNames = [];

    switch (type) {
        case "date": // format on Field component redux-form does not work
            if (input.value && input.value.length > 0) {
                input.value = moment(input.value).format(config.inputDateFormat);
            }            
            break;
        case "number":
            classNames.push("text-right");
            break;
        default:
    }

    return <div className={classnames("form-group", { "is-invalid": invalid })}>
        <InputGroup>
            <Input {...input} type={type} valid={touched ? valid : null} disabled={disabled} className={classNames.join(" ")} id={id}>
                {children}
            </Input>
            {label && <InputGroupAddon>{label}</InputGroupAddon>}            
        </InputGroup>
        {touched && error && <FormFeedback>{error}</FormFeedback>}
    </div>;
}

export default ValidatedInputFormField