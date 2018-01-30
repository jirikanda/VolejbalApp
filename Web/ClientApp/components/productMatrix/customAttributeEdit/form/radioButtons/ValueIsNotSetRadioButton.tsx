import * as React from 'react';
import { Field } from 'redux-form'
import ValueRadioButton, { ValueRadioButtonProps } from './ValueRadioButton';
import { CustomAttributeValueModelState } from '../../../../../models/CustomAttributeValue'

const ValueIsNotSetRadioButton = (props: ValueRadioButtonProps) =>
    <ValueRadioButton {...props} name={`${props.name}.valueState`} value={CustomAttributeValueModelState.IsNotSet} />

export default ValueIsNotSetRadioButton