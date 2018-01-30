import * as React from 'react';
import { Field } from 'redux-form'
import ValueRadioButton, { ValueRadioButtonProps } from './ValueRadioButton';
import { CustomAttributeValueModelState } from '../../../../../models/CustomAttributeValue'

const ValueIsSetRadioButton = (props: ValueRadioButtonProps) =>
    <ValueRadioButton {...props} name={`${props.name}.valueState`} value={CustomAttributeValueModelState.IsSet} />

export default ValueIsSetRadioButton