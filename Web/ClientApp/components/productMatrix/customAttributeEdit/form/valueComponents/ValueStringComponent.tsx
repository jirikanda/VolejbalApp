import * as React from 'react';
import { Field } from 'redux-form';
import ValidatedInputFormField from '../../../../form/ValidatedInputFormField';
import { ValueComponentProps } from './ValueComponent';
import InputFormField from '../../../../form/InputFormField';

interface ValueStringComponentProps extends ValueComponentProps {

}

const ValueStringComponent = (props: ValueStringComponentProps) => <Field
    key="ValueStringComponent"
    type="textarea"
    name={`${props.name}.valueString`}
    label={props.customAttribute.measureUnit}
    component={ValidatedInputFormField}
    onFocus={(event) => props.onValueChange(event, props.id)}
    onChange={(event) => props.onValueChange(event, props.id)}
    onClick={(event) => props.onValueChange(event, props.id)}
    disabled={props.disabled}
    required />

export default ValueStringComponent;